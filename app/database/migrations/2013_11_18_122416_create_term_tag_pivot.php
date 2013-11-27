<?php

use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateTermTagPivot extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::create('tag_term', function(Blueprint $table) {
            $table->integer('tag_id')->unsigned();
            $table->integer('term_id')->unsigned();
            
            $table->foreign('tag_id')->references('id')->on('tags');
            $table->foreign('term_id')->references('id')->on('terms');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::drop('tag_term');
    }

}
