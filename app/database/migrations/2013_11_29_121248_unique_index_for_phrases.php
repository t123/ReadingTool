<?php

use Illuminate\Database\Migrations\Migration;

class UniqueIndexForPhrases extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::table('terms', function($table) {
            $table->unique(array('user_id', 'language_id', 'phraseLower'));
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        //
    }

}
